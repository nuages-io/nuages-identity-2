var App =
    {
        data() {
            return {
                email: emailInitialValue,
                password: "",
                confirmPassword : "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            
            if (this.email === "")
                email.focus();
           else
            {
                password.focus();
            }
              
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
                            'Content-Type': 'application/json',
                            'X-Custom-RecaptchaToken' : token
                        },
                        body: JSON.stringify({
                                email: e,
                                password: p,
                                passwordConfirm: c,
                                code : code
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

                                self.errors = res.errors.map(function(m) {
                                    return { message : m}
                                });
                            }
                        });
                },
                resetPassword: function () {
                   
                    var self = this;
                    
                    this.errors = [];
                    formResetPassword.classList.remove("was-validated");

                    if (typeof email !== 'undefined')
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

                        if (typeof email !== 'undefined')
                        {
                            if (!email.validity.valid) {
                                if (email.validity.valueMissing) {
                                    email.setCustomValidity(emailRequiredMessage);
                                } else {
                                    email.setCustomValidity(emailInvalidMessage);
                                }
                            }
                        }

                        if (!password.validity.valid) {
                            password.setCustomValidity(passwordRequiredMessage);
                        }

                        if (!passwordConfirm.validity.valid) {
                            passwordConfirm.setCustomValidity(passwordConfirmRequiredMessage);
                        }

                        var list = formResetPassword.querySelectorAll("input:invalid");

                        list.forEach((element) => {

                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {

                this.errors = this.errors.filter(a => a.id !== "email");
               
                this.status = "";
                
                email.setCustomValidity("");
            },
            password(value) {
                this.errors = this.errors.filter(a => a.id !== "password");
                this.status = "";
                password.setCustomValidity("");
            },
            passwordConfirm(value) {
                this.errors = this.errors.filter(a => a.id !== "passwordConfirm");
                
                this.status = "";
                passwordCustom.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')