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
                doLoginSMS: function (token) {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";
                    
                    fetch("/api/account/sendSMSCode", {
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
                loginSMS: function () {

                    var self = this;
                    
                    this.errors = [];
                    formLoginSMS.classList.remove("was-validated");

                    email.setCustomValidity("");
                    
                    var res = formLoginSMS.checkValidity();
                    if (res) {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doforgotPassword(token);
                            });
                        });
                    } else {
                        formLoginSMS.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formLoginSMS.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
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