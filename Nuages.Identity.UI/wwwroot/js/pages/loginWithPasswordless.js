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
                doSendPasswordlessInfo: function (token) {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";

                    fetch("/api/account/passwordlessLogin", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken': token
                        },
                        body: JSON.stringify({
                                email: e
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
                sendPasswordlessInfo: function () {

                    var self = this;

                    this.errors = [];
                    formPasswordless.classList.remove("was-validated");

                    email.setCustomValidity("");

                    var res = formPasswordless.checkValidity();
                    if (res) {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doSendPasswordlessInfo(token);
                            });
                        });
                    } else {
                        formPasswordless.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formPasswordless.querySelectorAll("input:invalid");

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