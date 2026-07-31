import { SkyfishAuth } from "@limbo/skyfish/auth";

const baseUrl = "/umbraco/management/api/v1/limbo/skyfish";

// Makes an authenticated request to our own Management API endpoints
async function request(url, config) {

    if (!config) config = {};
    if (!config.method) config.method = "GET";
    if (!config.headers) config.headers = {};

    const token = await SkyfishAuth.TOKEN();
    config.headers.Authorization = `Bearer ${token}`;

    const res = await fetch(url, config);

    const contentType = res.headers.get("content-type") || "";

    if (contentType.includes("application/json")) {
        res.data = await res.json();
    } else if (contentType.startsWith("text/")) {
        res.textContent = await res.text();
    }

    if (!res.ok) throw res;

    return res;

}

async function get(url) {
    return await request(url);
}

// Returns the error message of a failed response, as our endpoints return the message as plain text
export function getErrorMessage(res) {
    if (!res) return null;
    if (res.textContent) return res.textContent;
    if (typeof res.data === "string") return res.data;
    if (res.data && res.data.message) return res.data.message;
    return null;
}

export class SkyfishService {

    static getServerVariables() {
        return get(`${baseUrl}/serverVariables`).then((res) => res.data);
    }

    static getVideo(source) {
        return get(`${baseUrl}/video?source=${encodeURIComponent(source)}`);
    }

    // Returns the first thumbnail of the video (the API currently only returns a single thumbnail)
    static getThumbnail(video) {
        return video && video.thumbnails && video.thumbnails.length > 0 ? video.thumbnails[0] : null;
    }

}

export default SkyfishService;
