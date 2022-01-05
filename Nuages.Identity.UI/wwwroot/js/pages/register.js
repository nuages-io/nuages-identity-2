var App =
    {
        data() {
            return {
                userNameOrEmail: "",
                password: "",
                passwordConfirm: "",
                errors: [],
                action: ""
            }
        },
        mounted() {
            userNameOrEmail.focus();
        },
        methods:
            {
                doRegister: function (token) {
                    var self = this;
                    var e = self.userNameOrEmail;
                    var p = self.password;
                    var pc = self.passwordConfirm;
                    var r = self.remember;

                    fetch("/api/account/register", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                userNameOrEmail: e,
                                password: p,
                                passwordConfirm: pc,
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                window.location = returnUrl;
                            } else
                                self.errors.push({message: res.message});
                        });

                },
                register: function () {
                    this.errors = [];
                    formLogin.classList.remove("was-validated");

                    userNameOrEmail.setCustomValidity("");
                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");

                    var res = formLogin.checkValidity();
                    if (res) {
                        var self = this;

                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doRegister(token);
                            });
                        });
                    } else {
                        formLogin.classList.add("was-validated");

                        if (!userNameOrEmail.validity.valid) {
                            if (userNameOrEmail.validity.valueMissing) {
                                userNameOrEmail.setCustomValidity(emailRequiredMessage);
                            } else {
                                userNameOrEmail.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        if (!password.validity.valid) {
                            password.setCustomValidity(passwordRequiredMessage);
                        }

                        if (!passwordConfirm.validity.valid) {
                            passwordConfirm.setCustomValidity(passwordConfirmationRequiredMessage);
                        }

                        var list = formLogin.querySelectorAll(":invalid");

                        list.forEach((element) => {

                            this.errors.push({
                                message: element.validationMessage,
                            });
                        });

                    }
                }
            },
        watch: {
            userNameOrEmail(value) {
                this.errors = [];
                this.action = "";
                userNameOrEmail.setCustomValidity("");
            },
            password(value) {
                this.errors = [];
                this.action = "";
                password.setCustomValidity("");
            },
            passwordConfirm(value) {
                this.errors = [];
                this.action = "";
                passwordConfirm.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')