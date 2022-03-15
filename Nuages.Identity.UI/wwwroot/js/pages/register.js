var App =
    {
        data() {
            return {
                email: "",
                password: "",
                passwordConfirm: "",
                errors: [],
                action: "",
                status: ""
            }
        },
        mounted() {
            email.focus();
        },
        methods:
            {
                doRegister: function (token) {
                    var self = this;
                    var e = self.email;
                    var p = self.password;
                    var pc = self.passwordConfirm;

                    fetch("/api/account/register", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken': token,
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                email: e,
                                password: p,
                                passwordConfirm: pc
                            }
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
                register: function () {

                    var self = this;

                    self.errors = [];
                    self.status = "sending";

                    formRegister.classList.remove("was-validated");

                    email.setCustomValidity("");
                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");

                    if (self.password !== self.passwordConfirm && self.passwordConfirm !== "") {
                        passwordConfirm.setCustomValidity(passwordMustMatch);
                    }

                    var res = formRegister.checkValidity();
                    if (res) {


                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doRegister(token);
                            });
                        });
                    } else {

                        self.status = "";

                        formRegister.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        if (!password.validity.valid) {
                            password.setCustomValidity(passwordRequiredMessage);
                        }

                        if (!passwordConfirm.validity.valid) {
                            if (passwordConfirm.validity.valueMissing)
                                passwordConfirm.setCustomValidity(passwordConfirmationRequiredMessage);
                        }

                        var list = formRegister.querySelectorAll("input:invalid");

                        list.forEach((element) => {

                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {
                this.errors = this.errors.filter(a => a.id !== "email");
                this.action = "";
                email.setCustomValidity("");
            },
            password(value) {
                this.errors = this.errors.filter(a => a.id !== "password");
                this.action = "";
                password.setCustomValidity("");
            },
            passwordConfirm(value) {
                this.errors = this.errors.filter(a => a.id !== "passwordConfirm");
                this.action = "";
                passwordConfirm.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')