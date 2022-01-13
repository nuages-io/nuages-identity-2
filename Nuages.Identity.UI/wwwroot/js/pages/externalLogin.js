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
                doExternalLogin: function (token) {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";

                    fetch("/api/account/externalLogin", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                email: e,
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            self.status = "done";

                            if (res.success) {

                            } else
                                self.errors.push({message: res.message});
                        });

                },
                externalLogin: function () {

                    var self = this;

                    this.errors = [];
                    formExternalLogin.classList.remove("was-validated");


                    var res = formExternalLogin.checkValidity();
                    if (res) {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doExternalLogin(token);
                            });
                        });
                    } else {
                        formExternalLogin.classList.add("was-validated");

                        var list = formExternalLogin.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            }

    };

Vue.createApp(App).mount('#app')