var App =
    {
        data() {
            return {
                password: "",
                confirmPassword : "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            password.focus();              
        },
        methods:
            {
                doSetPassword: function () {
                    var self = this;                    
                    
                    var p = self.password;
                    var c = self.passwordConfirm;
                                                           
                    fetch("/api/manage/setPassword", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                newPassword: p,
                                newPasswordConfirm: c
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
                setPassword: function () {
                   
                    var self = this;
                    
                    this.errors = [];
                    formSetPassword.classList.remove("was-validated");
                                     
                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");
                    
                    var res = formSetPassword.checkValidity();
                    if (res) {

                        this.status = "sending";

                        self.doSetPassword();
                    } else {

                        formSetPassword.classList.add("was-validated");

                        if (!password.validity.valid) {
                            password.setCustomValidity(passwordRequiredMessage);
                        }

                        if (!passwordConfirm.validity.valid) {
                            passwordConfirm.setCustomValidity(passwordConfirmRequiredMessage);
                        }

                        var list = formSetPassword.querySelectorAll(":invalid");

                        list.forEach((element) => {

                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            },
        watch: {
          
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