import { html, css, when, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";
import { UmbFormControlMixin } from "@umbraco-cms/backoffice/validation";

import "@limbo/video/elements/duration";
import { SkyfishService, getErrorMessage } from "@limbo/skyfish/service";

// As Umbraco/JSON.net will corrupt any timestamps in the JSON, the video details are stored as a serialized string
// in a "_data" property rather than as a nested object
function parseDetails(details) {
    if (!details || typeof details !== "object" || !details._data) return null;
    try {
        return JSON.parse(details._data);
    } catch {
        return null;
    }
}

function serializeDetails(details) {
    return { _data: JSON.stringify(details) };
}

class LimboSkyfishVideoElement extends UmbFormControlMixin(UmbLitElement, undefined) {

    static properties = {
        readonly: { type: Boolean, reflect: true },
        mandatory: { type: Boolean },
        mandatoryMessage: { type: String }
    };

    static styles = css`

        :host {
            display: block;
            position: relative;
        }

        /* [CHANGE: code review fix - "pointer-events: none" made the whole editor unclickable for the entire
           lookup (which polls Skyfish for up to two minutes), so the user couldn't even press "Clear". The
           loading state now only dims the editor, like it did in v13.]
           Related: Controllers/SkyfishManagementController.cs, wwwroot/Icons/Skyfish.js */
        .shell.loading > div {
            opacity: 0.6;
        }

        .shell.loading uui-loader {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translateX(-50%);
        }

        h5 {
            margin: 0 0 var(--uui-size-space-2);
        }

        .source {
            width: 100%;
            max-width: 1100px;
            box-sizing: border-box;
        }

        uui-textarea.source {
            --uui-textarea-min-height: 7rem;
        }

        .actions {
            display: flex;
            flex-wrap: wrap;
            gap: var(--uui-size-space-3);
            margin-top: var(--uui-size-space-3);
        }

        .error {
            margin-top: var(--uui-size-space-3);
            max-width: 1100px;
            color: var(--uui-color-danger);
        }

        .block {
            margin-top: var(--uui-size-layout-1);
        }

        .box {
            position: relative;
            display: flex;
            flex-wrap: wrap;
            gap: var(--uui-size-space-4);
            padding: var(--uui-size-space-4);
            border: 1px solid var(--uui-color-border);
            border-radius: var(--uui-border-radius);
            background: var(--uui-color-surface-alt);
            box-sizing: border-box;
            max-width: 1100px;
        }

        .thumbnail {
            flex: 0 0 240px;
            max-width: 240px;
            border-radius: var(--uui-border-radius);
            overflow: hidden;
            background: var(--uui-color-surface);
        }

        .thumbnail img {
            display: block;
            width: 100%;
            height: auto;
        }

        .info {
            flex: 1;
            min-width: 240px;
        }

        dl {
            display: grid;
            grid-template-columns: auto 1fr;
            gap: 2px var(--uui-size-space-5);
            margin: 0;
        }

        dt {
            color: var(--uui-color-text-alt);
            font-weight: 600;
            white-space: nowrap;
        }

        dd {
            margin: 0;
        }

        .app-url {
            position: absolute;
            top: var(--uui-size-space-3);
            right: var(--uui-size-space-3);
            color: var(--uui-color-text-alt);
            font-size: 18px;
        }

        .description {
            margin-top: var(--uui-size-space-4);
            color: var(--uui-color-text-alt);
            white-space: pre-wrap;
        }

    `;

    // [CHANGE: code review fixes - derive the UI state from the incoming value, guard against stale lookups, wire up mandatory validation and keep the focus when the input is swapped] Related: wwwroot/EntryPoint.js, Api/SkyfishSecurityFilter.cs, SkyfishPackage.cs

    #details = null;
    #loading = false;
    #error = "";
    #value = null;
    #embedMode = false;
    #debounceTimer = 0;
    #formControl = null;
    #internalUpdate = false;
    #requestId = 0;
    #pendingFocus = false;

    constructor() {

        super();

        // Without this validator the "mandatory" and "mandatoryMessage" properties would have no effect, and the
        // user wouldn't get any inline feedback when leaving a mandatory property empty
        this.addValidator(
            "valueMissing",
            () => this.mandatoryMessage ?? "#validation_invalidEmpty",
            () => this.mandatory === true && !this.#sourceValue()
        );

    }

    get value() {
        return this.#value;
    }

    set value(value) {

        const oldValue = this.#value;
        this.#value = value ?? undefined;

        // The value may be (re)assigned by Umbraco at any time - eg. on the initial render, when the user discards
        // their changes or when switching between variants - so the UI state must be derived from the new value
        // rather than just once when the element is connected. Values committed by the element itself are skipped as
        // the state has then already been updated.
        if (!this.#internalUpdate) {
            this.#details = parseDetails(this.#value?.details);
            this.#embedMode = this.#isEmbedCode(this.#value?.source);
        }

        this.requestUpdate("value", oldValue);

    }

    // The source is entered in either an input or a textarea depending on whether the user pasted an embed code, so
    // the currently rendered element must be (re)registered as the form control of this property editor
    updated(changedProperties) {

        super.updated(changedProperties);

        const control = this.renderRoot.querySelector(".source");

        if (control !== this.#formControl) {

            if (this.#formControl) this.removeFormControlElement(this.#formControl);

            this.#formControl = control;

            if (control) this.addFormControlElement(control);

        }

        // When the input and the textarea are swapped, the element the user was typing in is removed from the DOM,
        // so the focus has to be moved to the new element
        if (control && this.#pendingFocus) {
            this.#pendingFocus = false;
            control.focus();
        }

    }

    disconnectedCallback() {
        super.disconnectedCallback();
        window.clearTimeout(this.#debounceTimer);
    }

    #currentValue() {
        return this.value && typeof this.value === "object" ? this.value : null;
    }

    #sourceValue() {
        return this.#currentValue()?.source ?? "";
    }

    #isEmbedCode(source) {
        return !!source && source.indexOf("<") >= 0;
    }

    #commit(value) {
        this.#internalUpdate = true;
        try {
            this.value = value;
        } finally {
            this.#internalUpdate = false;
        }
        this.dispatchEvent(new UmbChangeEvent());
    }

    #clear() {
        window.clearTimeout(this.#debounceTimer);
        this.#requestId++;
        this.#details = null;
        this.#error = "";
        this.#loading = false;
        this.#embedMode = false;
        this.#commit(undefined);
    }

    async #lookup(source) {

        // A lookup may take up to a few minutes as the Skyfish API is polled while the video is being transcoded, so
        // the response of an outdated lookup must not overwrite the result of a newer one
        const requestId = ++this.#requestId;

        this.#loading = true;
        this.#error = "";
        this.requestUpdate();

        try {

            const res = await SkyfishService.getVideo(source);

            if (requestId !== this.#requestId) return;

            this.#details = res.data.details;

            this.#commit({
                source: source,
                credentials: res.data.credentials,
                details: serializeDetails(res.data.details),
                embed: res.data.embed
            });

        } catch (res) {

            if (requestId !== this.#requestId) return;

            this.#details = null;
            this.#error = getErrorMessage(res) ?? "An error occured on the server.";

            // Keep the source so the user can correct it, but drop the video information
            this.#commit({ source: source });

        } finally {

            if (requestId === this.#requestId) {
                this.#loading = false;
                this.requestUpdate();
            }

        }

    }

    #scheduleLookup(source) {

        window.clearTimeout(this.#debounceTimer);
        this.#requestId++;

        if (!source) {
            this.#clear();
            return;
        }

        this.#loading = true;
        this.#error = "";
        this.requestUpdate();

        this.#debounceTimer = window.setTimeout(() => this.#lookup(source), 400);

    }

    #onSourceInput(event) {

        const source = event.target.value ?? "";

        // Switch between the single line input and the textarea depending on whether the user is pasting an embed code
        const embedMode = this.#isEmbedCode(source);
        if (embedMode !== this.#embedMode) {
            this.#embedMode = embedMode;
            this.#pendingFocus = true;
            this.requestUpdate();
        }

        this.#commit({ ...(this.#currentValue() ?? {}), source: source });
        this.#scheduleLookup(source.trim());

    }

    #onRefresh() {
        const source = this.#sourceValue().trim();
        if (!source) {
            this.#clear();
            return;
        }
        this.#lookup(source);
    }

    #renderInput(source) {

        const label = this.localize.term("limboSkyfish_urlOrEmbedCode");
        const placeholder = this.localize.term("limboSkyfish_urlPlaceholder");

        if (this.#embedMode) {
            return html`
                <uui-textarea
                    class="source"
                    label=${label}
                    .value=${source}
                    ?disabled=${this.readonly}
                    placeholder=${placeholder}
                    @input=${this.#onSourceInput}></uui-textarea>
            `;
        }

        return html`
            <uui-input
                class="source"
                type="text"
                label=${label}
                .value=${source}
                ?disabled=${this.readonly}
                placeholder=${placeholder}
                @input=${this.#onSourceInput}></uui-input>
        `;

    }

    #renderEditor(source) {
        return html`
            <div>
                <h5><umb-localize key="limboSkyfish_urlOrEmbedCode">URL or embed code</umb-localize></h5>
                ${this.#renderInput(source)}
                <div class="actions">
                    <uui-button
                        look="outline"
                        label=${this.localize.term("limboSkyfish_refresh")}
                        ?disabled=${this.readonly || !source.trim()}
                        @click=${this.#onRefresh}></uui-button>
                    <uui-button
                        look="outline"
                        label=${this.localize.term("limboSkyfish_clear")}
                        ?disabled=${this.readonly || !source}
                        @click=${this.#clear}></uui-button>
                </div>
            </div>
        `;
    }

    #renderError() {
        if (!this.#error) return nothing;
        // The message is shown inline next to the input rather than as a notification, as it is feedback on the
        // value the user is currently entering
        return html`<div class="error" role="alert">${this.#error}</div>`;
    }

    #renderDetails() {

        const details = this.#details;

        if (!details) return nothing;

        const thumbnail = SkyfishService.getThumbnail(details);
        const title = details.title ? details.title : details.fileName;
        const appUrl = details.folderId ? `https://app.skyfish.com/folder/${details.folderId}/file/${details.uniqueMediaId}` : null;

        return html`
            <div class="block">
                <h5><umb-localize key="limboSkyfish_video">Video</umb-localize></h5>
                <div class="box">
                    ${when(appUrl, () => html`
                        <a
                            class="app-url"
                            href=${appUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            title=${this.localize.term("limboSkyfish_appUrlTooltip")}><uui-icon name="icon-out"></uui-icon></a>
                    `)}
                    ${when(thumbnail, () => html`
                        <div class="thumbnail">
                            <img src=${thumbnail.url} width=${thumbnail.width} height=${thumbnail.height} alt=${title ?? ""} />
                        </div>
                    `)}
                    <div class="info">
                        <dl>
                            <dt><umb-localize key="limboSkyfish_id">ID</umb-localize></dt>
                            <dd><code>${details.uniqueMediaId}</code></dd>
                            <dt><umb-localize key="limboSkyfish_title">Title</umb-localize></dt>
                            <dd>${title}</dd>
                            <dt><umb-localize key="limboSkyfish_duration">Duration</umb-localize></dt>
                            <dd>${when(details.duration, () => html`<limbo-video-duration .value=${details.duration}></limbo-video-duration>`)}</dd>
                        </dl>
                        ${when(details.description, () => html`<div class="description">${details.description}</div>`)}
                    </div>
                </div>
            </div>
        `;

    }

    render() {
        const source = this.#sourceValue();
        return html`
            <div class="shell ${this.#loading ? "loading" : ""}">
                <div>
                    ${this.#renderEditor(source)}
                    ${this.#renderError()}
                    ${this.#renderDetails()}
                </div>
                ${this.#loading ? html`<uui-loader></uui-loader>` : nothing}
            </div>
        `;
    }

}

customElements.define("limbo-skyfish-video", LimboSkyfishVideoElement);

export { LimboSkyfishVideoElement as element };
export default LimboSkyfishVideoElement;
