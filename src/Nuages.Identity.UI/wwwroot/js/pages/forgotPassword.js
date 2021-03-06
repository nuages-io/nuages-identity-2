var App =
    {
        data() {
            return {
                email: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            email.focus();
        },
        methods:
            {
                doforgotPassword: function (token) {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";

                    fetch("/app/account/forgotPassword", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken': token,
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                email: e
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            self.email = null;
                            self.status = "done";

                            if (!res.success)
                                self.errors.push({message: res.message});
                        });

                },
                forgotPassword: function () {

                    var self = this;

                    this.errors = [];
                    formforgotPassword.classList.remove("was-validated");

                    email.setCustomValidity("");

                    var res = formforgotPassword.checkValidity();
                    if (res) {
                        if (recaptchaToken !== "") {
                            grecaptcha.ready(function () {
                                grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                    self.doforgotPassword(token);
                                });
                            });
                        }
                        else
                        {
                            self.doforgotPassword("");
                        }
                    } else {
                        formforgotPassword.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formforgotPassword.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {
                this.errors = [];
                if (value != null) {
                    this.status = "";
                }

                if (typeof (email) != "undefined")
                    email.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')