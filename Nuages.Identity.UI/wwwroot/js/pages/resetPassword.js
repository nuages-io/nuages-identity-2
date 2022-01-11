var App =
    {
        data() {
            return {
                email: "",
                password: "",
                confirmPassword : "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            email.focus();
        },
        methods:
            {
                doResetPassword: function (token) {
                    var self = this;
                    var e = self.email;
                    var p = self.password;
                    var c = self.passwordConfirm;
                    
                   
                    
                    fetch("/api/account/resetPassword", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                email: e,
                                password: p,
                                passwordConfirm: c,
                                 code : code,
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                           
                            
                            if (res.success) {
                                self.status = "done";
                                self.errors = [];
                            }
                            else
                            {
                                self.status = "error";
                                var err = res.errors.map(function(m) {
                                    return { message : m}
                                });
                                self.errors = err;
                                if (res.message)
                                {
                                    self.errors.push({message: res.message});
                                }
                            }
                                
                        });

                },
                resetPassword: function () {
                   
                    var self = this;
                    
                    this.errors = [];
                    formResetPassword.classList.remove("was-validated");

                    email.setCustomValidity("");
                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");
                    
                    var res = formResetPassword.checkValidity();
                    if (res) {

                        this.status = "sending";
                        
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doResetPassword(token);
                            });
                        });
                    } else {
                        
                        formResetPassword.classList.add("was-validated");

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
                            passwordConfirm.setCustomValidity(passwordConfirmRequiredMessage);
                        }

                        var list = formResetPassword.querySelectorAll(":invalid");

                        list.forEach((element) => {

                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {

                this.errors.splice(this.errors.findIndex( a => a.id === "email"), 1);
               
                this.status = "";
                
                email.setCustomValidity("");
            },
            password(value) {
                this.errors.splice(this.errors.findIndex( a => a.id === "password"), 1);
                
                this.status = "";
                password.setCustomValidity("");
            },
            passwordConfirm(value) {
                this.errors.splice(this.errors.findIndex( a => a.id === "passwordConfirm"), 1);
                
                this.status = "";
                passwordCustom.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')