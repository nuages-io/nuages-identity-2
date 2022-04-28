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
                doSendMagicLinkInfo: function (token) {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";

                    fetch("/app/account/magicLinkLogin", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken': token,
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                email: e,
                                returnUrl: returnUrl
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            self.status = "done";

                            if (!res.success) {
                                self.errors.push({message: res.message});
                            }
                        });

                },
                sendMagicLinkInfo: function () {

                    var self = this;

                    this.errors = [];
                    formMagicLink.classList.remove("was-validated");

                    email.setCustomValidity("");

                    var res = formMagicLink.checkValidity();
                    if (res) {

                        if (recaptchaToken !== "") {
                            grecaptcha.ready(function () {
                                grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                    self.doSendMagicLinkInfo(token);
                                });
                            });
                        }
                        else
                        {
                            self.doSendMagicLinkInfo("");
                        }
                    } else {
                        formMagicLink.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formMagicLink.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {
                this.errors = [];
                this.status = "";
                email.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')