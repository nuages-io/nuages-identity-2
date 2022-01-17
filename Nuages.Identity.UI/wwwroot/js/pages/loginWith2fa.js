var App =
    {
        data() {
            return {
                code: "",            
                errors: [],
                remember: rememberMe,
                rememberMachine: false,
                action: "",
                status : ""
            }
        },
        mounted() {
            code.focus();
            setTimeout(function()
            {
                code.value = "";
            })
        },
        methods:
            {
                doLogin: function (token) {
                    var self = this;
                    var c = self.code;
                    var r = self.remember;
                    var m = self.rememberMachine;
                    
                    fetch("/api/account/login2fa", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                code: c,
                                rememberMe: r,
                                rememberMachine: m,
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                                
                            if (res.success) {
                                window.location = returnUrl;
                            } else
                                
                                switch (res.reason) {
                                    // case "PasswordExpired":
                                    // case "PasswordMustBeChanged": 
                                    // {
                                    //     window.location = "/account/resetpassword?expired=true";
                                    //     break;
                                    // }
                                    // case "MfaRequired": {
                                    //     window.location = "/account/loginwith2fa";
                                    //     break;
                                    // }
                                    // case "EmailNotConfirmed": {
                                    //     window.location = "/account/emailnotconfirmed";
                                    //     break;
                                    // }
                                    // case "PhoneNotConfirmed": {
                                    //     window.location = "/account/phonenotconfirmed";
                                    //     break;
                                    // }                                   
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

                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doLogin(token);
                            });
                        });
                    } else {

                        
                        formLogin.classList.add("was-validated");

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formLogin.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });
                    }
                }
            },
        watch: {
            code(value) {
                this.errors.splice(this.errors.findIndex( a => a.id === "code"), 1);
                this.action = "";
                code.setCustomValidity("");
            },
           
        }
    };

Vue.createApp(App).mount('#app')