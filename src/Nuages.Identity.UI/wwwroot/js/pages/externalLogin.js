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
                doExternalLogin: function (token) {
                    var self = this;

                    fetch("/app/account/registerExternalLogin", {
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
                            if (res.success) {
                                if (res.showConfirmationMessage) {
                                    self.status = "done";
                                } else {
                                    window.location = returnUrl;
                                }
                            } else {
                                self.status = "";

                                res.errors.forEach((element) => {
                                    self.errors.push({message: element});
                                });
                            }
                        });

                },
                externalLogin: function () {

                    var self = this;

                    self.errors = [];
                    self.status = "sending";

                    formExternalLogin.classList.remove("was-validated");

                    var res = formExternalLogin.checkValidity();
                    if (res) {
                        if (recaptchaToken !== "") {
                            grecaptcha.ready(function () {
                                grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                    self.doExternalLogin(token);
                                });
                            });
                        }
                        else {
                            self.doExternalLogin("");
                        }
                    } else {

                        self.status = "";

                        formExternalLogin.classList.add("was-validated");

                        var list = formExternalLogin.querySelectorAll("input:invalid");

                        list.forEach((element) => {

                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            }
    };

Vue.createApp(App).mount('#app')