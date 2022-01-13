var App =
    {
        data() {
            return {
                errors: [],
                status : ""
            }
        },
        methods:
            {
                doExternalLogin: function (token) {
                    var self = this;

                    fetch("/api/account/registerExternalLogin", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                             
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                if (res.showConfirmationMessage)
                                {
                                    self.status = "done";
                                }
                                else
                                {
                                    window.location = returnUrl;
                                }

                            }
                            else
                            {
                                self.status = "";
                                self.errors.push({message: res.errorMessage});
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


                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doExternalLogin(token);
                            });
                        });
                    } else {

                        self.status = "";

                        formExternalLogin.classList.add("was-validated");

                        var list = formExternalLogin.querySelectorAll(":invalid");

                        list.forEach((element) => {

                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            }
    };

Vue.createApp(App).mount('#app')