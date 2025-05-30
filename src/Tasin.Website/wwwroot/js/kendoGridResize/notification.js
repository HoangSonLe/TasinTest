var notification = $("#notification")
    .kendoNotification({
        position: {
            pinned: true,
            bottom: 20,
            right: 20,
        },
        autoHideAfter: 5000,
        stacking: "up",
        animation: {
            open: {
                effects: "slideIn:right",
                duration: 400,
            },
            close: {
                effects: "slideIn:right",
                reverse: true,
                duration: 300,
            },
        },
        templates: [
            {
                type: "info",
                template: $("#wrongTemplate").html(),
            },
            {
                type: "error",
                template: $("#errorTemplate").html(),
            },
            {
                type: "success",
                template: $("#successTemplate").html(),
            },
        ],
        show: function (e) {
            // Add custom class for higher z-index
            $(".k-notification").closest(".k-animation-container").addClass("customClass");

            // Add progress bar animation
            var progressBar = e.element.find(".notification-progress");
            if (progressBar.length > 0) {
                progressBar.css("animation-duration", "5s");
            }
        },
    })
    .data("kendoNotification");
