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

                    var self = this;
                    
                    this.errors = [];
                    formRegister.classList.remove("was-validated");

                    userNameOrEmail.setCustomValidity("");
                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");

                    if (self.password !== self.passwordConfirm && self.passwordConfirm !== "")
                    {
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
                        formRegister.classList.add("was-validated");

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
                            if (passwordConfirm.validity.valueMissing)
                                passwordConfirm.setCustomValidity(passwordConfirmationRequiredMessage);
                        }

                        var list = formRegister.querySelectorAll(":invalid");

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