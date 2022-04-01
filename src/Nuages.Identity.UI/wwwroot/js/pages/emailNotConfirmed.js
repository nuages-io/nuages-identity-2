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
                doSend: function(token)
                {
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
                   
                },
                send: function () {
                    var self = this;

                    this.status = "sending";

                    if (recaptcha !== "")
                    {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                doSend(token);
                            });
                        });
                    }
                    else {
                        doSend("");
                    }
                   


                }
            }
    };

Vue.createApp(App).mount('#app')