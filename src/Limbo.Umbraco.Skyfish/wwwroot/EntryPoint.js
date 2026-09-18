import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

import { SkyfishAuth } from "@limbo/skyfish/auth";
import { SkyfishPackage } from "@limbo/skyfish/package";
import { SkyfishService } from "@limbo/skyfish/service";

const PACKAGE_ALIAS = "Limbo.Umbraco.Skyfish";

const PACKAGE_NAME = "Limbo Skyfish";

// The schema alias must match the alias of the "SkyfishVideoPropertyEditor" data editor
const SCHEMA_ALIAS = "Limbo.Umbraco.Skyfish.Video";

const UI_ALIAS = "Limbo.Umbraco.Skyfish.Video.Ui";

// [CHANGE: code review fix - the context callback may run more than once, and a failed server variables request would otherwise leave the package silently unregistered] Related: wwwroot/Elements/Video.js, Api/SkyfishSecurityFilter.cs, SkyfishPackage.cs
let initialized = false;

export const onInit = (host, extensionRegistry) => {

    host.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {

        if (!authContext) return;

        const config = authContext.getOpenApiConfiguration();
        SkyfishAuth.TOKEN = config.token;

        // The context callback runs again whenever the auth context is replaced, but the extensions must only ever
        // be registered once as the registry rejects duplicate aliases
        if (initialized) return;
        initialized = true;

        // The cache buster used when registering the extensions below comes from the server, so we need to await the
        // server variables before we can register anything
        SkyfishService.getServerVariables().then((serverVariables) => {
            SkyfishPackage.serverVariables = serverVariables;
        }).catch((error) => {
            // Without this, a failed request (eg. if the user doesn't have access to the content section) would
            // result in an unhandled rejection, and none of the package's extensions would be registered
            initialized = false;
            console.error("[Limbo.Umbraco.Skyfish] Failed loading the server variables of the package.", error);
        });

    });

};