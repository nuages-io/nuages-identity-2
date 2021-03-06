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
                doLoginSMS: function (token) {
                    var self = this;

                    this.status = "sending";

                    fetch("/app/account/sendSMSCode", {
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

                            if (res.success) {
                                window.location = "/Account/LoginWithSMS?returnUrl=" + returnUrlEncoded;
                            } else
                                self.errors = res.errors.map(function (m) {
                                    return {message: m}
                                });
                        });

                },
                loginSMS: function () {

                    var self = this;

                    this.errors = [];
                    formLoginSMS.classList.remove("was-validated");


                    var res = formLoginSMS.checkValidity();
                    if (res) {

                        if (recaptchaToken !== "") {
                            grecaptcha.ready(function () {
                                grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                    self.doLoginSMS(token);
                                });
                            });
                        }
                        else
                        {
                            self.doLoginSMS("");
                        }
                    } else {
                        formLoginSMS.classList.add("was-validated");


                        var list = formLoginSMS.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            }
    };

Vue.createApp(App).mount('#app')