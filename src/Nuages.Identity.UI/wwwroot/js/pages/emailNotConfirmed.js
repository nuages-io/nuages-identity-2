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
                    var self = this;
                    
                    fetch("/app/account/sendEmailConfirmation",
                        {
                            method: "POST",
                            headers: {
                                'Content-Type': 'application/json',
                                'X-Custom-RecaptchaToken': token,
                                "X-XSRF-TOKEN": xsrfToken
                            },
                            body: JSON.stringify({}
                            )
                        })
                        .then(response => response.json())
                        .then(res => {
                            self.status = "done";
                        });
                   
                },
                send: function () {
                    var self = this;

                    this.status = "sending";

                    if (recaptchaToken !== "")
                    {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                doSend(token);
                            });
                        });
                    }
                    else {
                        this.doSend("");
                    }
                   


                }
            }
    };

Vue.createApp(App).mount('#app')