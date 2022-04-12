var App =
    {
        data() {
            return {
                code: "",
                errors: [],
                action: "",
                status: ""
            }
        },
        mounted() {
            code.focus();
            setTimeout(function () {
                code.value = "";
            })
        },
        methods:
            {
                doLogin: function (token) {
                    var self = this;
                    var c = self.code;

                    fetch("/api/account/loginSMS", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken': token,
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                code: c
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                window.location = returnUrl;
                            } else

                                switch (res.reason) {

                                    default: {
                                        this.status = "";
                                        //NotWithinDateRange,
                                        //AccountNotConfirmed,
                                        //PasswordNeverSet,
                                        //RecaptchaError,
                                        //LockedOut
                                        self.errors.push({message: res.message});
                                        break;
                                    }
                                }
                        });

                },
                login: function () {

                    this.errors = [];
                    formLogin.classList.remove("was-validated");

                    code.setCustomValidity("");

                    var res = formLogin.checkValidity();
                    if (res) {

                        this.status = "sending";
                        var self = this;

                        if (recaptchaToken !== "") {
                            grecaptcha.ready(function () {
                                grecaptcha.execute(recaptchaToken, {action: 'submit'}).then(function (token) {
                                    self.doLogin(token);
                                });
                            });
                        }
                        else {
                            self.doLogin("");
                        }
                    } else {


                        formLogin.classList.add("was-validated");

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formLogin.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                }
            },
        watch: {
            code(value) {
                this.errors = this.errors.filter(a => a.id !== "code");
                this.action = "";
                code.setCustomValidity("");
            },

        }
    };

Vue.createApp(App).mount('#app')