var App =
    {
        data() {
            return {
                code: "",               
                errors: [],              
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
                                   
                    fetch("/api/account/loginRecoveryCode", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                code: c,
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