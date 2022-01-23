var App =
    {
        data() {
            return {
                currentPassword: "",
                password: "",
                confirmPassword: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            currentPassword.focus();
        },
        methods:
            {
                doChangePassword: function () {
                    var self = this;

                    var cp = self.currentPassword;
                    var p = self.password;
                    var c = self.passwordConfirm;


                    fetch("/api/manage/changePassword", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                currentPassword: cp,
                                newPassword: p,
                                newPasswordConfirm: c
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                
                                
                                self.currentPassword = null;
                                self.password = null;
                                self.passwordConfirm = null;

                                self.status = "done";
                                self.errors = [];
                                
                                setTimeout(function()
                                {
                                    currentPassword.focus();
                                });
                            } else {
                                self.status = "error";
                                self.errors = res.errors.map(function (m) {
                                    return {message: m}
                                });
                            }
                        });

                },
                changePassword: function () {

                    var self = this;
                    this.status = "";
                    
                    this.errors = [];
                    formChangePassword.classList.remove("was-validated");

                    currentPassword.setCustomValidity("");

                    password.setCustomValidity("");
                    passwordConfirm.setCustomValidity("");

                    var res = formChangePassword.checkValidity();
                    if (res) {

                        this.status = "sending";

                        self.doChangePassword();
                    } else {

                        formChangePassword.classList.add("was-validated");

                        if (!currentPassword.validity.valid) {
                            currentPassword.setCustomValidity(currentPasswordRequiredMessage);
                        }

                        if (!password.validity.valid) {
                            password.setCustomValidity(passwordRequiredMessage);
                        }

                        if (!passwordConfirm.validity.valid) {
                            passwordConfirm.setCustomValidity(passwordConfirmRequiredMessage);
                        }

                        var list = formChangePassword.querySelectorAll(":invalid");

                        list.forEach((element) => {

                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                }
            },
        watch: {
            currentPassword(value) {

                this.errors.splice(this.errors.findIndex(a => a.id === "currentPassword"), 1);

                if (value != null)
                    this.status = "";

                currentPassword.setCustomValidity("");
            },
            password(value) {
                this.errors.splice(this.errors.findIndex(a => a.id === "password"), 1);

                if (value != null)
                    this.status = "";
                
                password.setCustomValidity("");
            },
            passwordConfirm(value) {
                this.errors.splice(this.errors.findIndex(a => a.id === "passwordConfirm"), 1);

                if (value != null)
                    this.status = "";
                
                passwordCustom.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')