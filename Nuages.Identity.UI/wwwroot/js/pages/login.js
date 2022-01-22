var App =
    {
        data() {
            return {
                userNameOrEmail: "",
                password: "",
                errors: [],
                remember: false,
                action: "",
                status : ""
            }
        },
        mounted() {
            userNameOrEmail.focus();
            setTimeout(function()
            {
                userNameOrEmail.value = "";
            })
        },
        methods:
            {
                doLogin: function (token) {
                    var self = this;
                    var e = self.userNameOrEmail;
                    var p = self.password;
                    var r = self.remember;

                    fetch("/api/account/login", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                userNameOrEmail: e,
                                password: p,
                                recaptchaToken: token,
                                rememberMe: r
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                           
                            
                            if (res.success) {
                                window.location = returnUrl;
                            } else
                                
                                switch (res.reason) {
                                    case "PasswordExpired":
                                    case "PasswordMustBeChanged": 
                                    {
                                        window.location = "/account/resetpassword?expired=true";
                                        break;
                                    }
                                    case "EmailNotConfirmed": {
                                        window.location = "/account/emailnotconfirmed";
                                        break;
                                    }
                                    case "PhoneNotConfirmed": {
                                        window.location = "/account/phonenotconfirmed";
                                        break;
                                    }                                   
                                    default: {
                                        
                                        if (res.result.requiresTwoFactor === true)
                                        {
                                            window.location = "/account/loginwith2fa?returnUrl=" + returnUrl;
                                            break;
                                        }
                                        
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

                    userNameOrEmail.setCustomValidity("");
                    password.setCustomValidity("");

                    var res = formLogin.checkValidity();
                    if (res) {

                        this.status = "sending";
                        var self = this;

                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doLogin(token);
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

                        var list = formLogin.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });
                    }
                }
            },
        watch: {
            userNameOrEmail(value) {
                this.errors.splice(this.errors.findIndex( a => a.id === "userNameOrEmail"), 1);
                this.action = "";
                userNameOrEmail.setCustomValidity("");
            },
            password(value) {
                this.errors.splice(this.errors.findIndex( a => a.id === "password"), 1);
                this.action = "";
                password.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')