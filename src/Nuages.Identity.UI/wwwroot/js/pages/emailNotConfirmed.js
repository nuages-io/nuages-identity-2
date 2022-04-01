var App =
    {
        data() {
            return {
                errors: [],
                status: ""
            }
        },
        methods:
            {
                send: function () {
                    var self = this;

                    this.status = "sending";

                    grecaptcha.ready(function () {
                        grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                            fetch("/api/account/sendEmailConfirmation",
                                {
                                    method: "POST",
                                    headers: {
                                        'Content-Type': 'application/json',
                                        'X-Custom-RecaptchaToken': token,
                                        "X-XSRF-TOKEN": xsrfToken
                                    },
                                    body: JSON.stringify({}
                                    )
                                }).then(response => response.json())
                                .then(res => {
                                    self.status = "done";
                                });
                        });
                    });


                }
            }
    };

Vue.createApp(App).mount('#app')