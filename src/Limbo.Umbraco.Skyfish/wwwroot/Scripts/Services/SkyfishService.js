angular.module("umbraco.services").factory("skyfishService", function ($http, limboVideoService) {

    // Get relevant settings from Umbraco's server variables
    const cacheBuster = Umbraco.Sys.ServerVariables.application.cacheBuster;
    const umbracoPath = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath;

    // Fetches information about the video from our underlying API
    function getVideo(source) {

        const data = {
            source: source
        };

        return $http({
            method: "POST",
            url: `${umbracoPath}/backoffice/Limbo/Skyfish/GetVideo`,
            umbIgnoreErrors: true,
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            data: $.param(data)
        });

    }

    // Returns a thumbnail object for the video
    function getThumbnail(video) {
       return video && video.thumbnails && video.thumbnails.length > 0 ? video.thumbnails[0] : null;
    }

    return {
        getVideo: getVideo,
        getThumbnail: getThumbnail,
        getDuration: function (seconds) {
            if (!seconds) return null;
            if (typeof seconds === "object" && seconds.duration) seconds = seconds.duration;
            return limboVideoService.getDuration(seconds);
        }
    }

});