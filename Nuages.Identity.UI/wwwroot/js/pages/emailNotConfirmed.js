var App =
    {
        data(){
            return {
                errors: [],
                status : ""
            }
        },
        methods:
            {
                send : function()
                {
                    this.status = "sending";

                    grecaptcha.ready(function () {
                        grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                            fetch("/api/account/sendEmailConfirmation",
                                {
                                    method: "POST",
                                    headers: {
                                        'Content-Type': 'application/json'
                                    },
                                    body: JSON.stringify({
                                            recaptchaToken: token,
                                        }
                                    )
                                }).then(response => response.json())
                                .then(res => {
                                    this.status = "sent";
                                });
                        });
                    });
                    
                            
                }
            }
    };

Vue.createApp(App).mount('#app')