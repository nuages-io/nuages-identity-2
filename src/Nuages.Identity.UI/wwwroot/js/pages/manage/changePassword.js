var App =
    {
        data() {
            return {
                currentPassword: "",
                password: "",
                passwordConfirm: "",
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
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
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

                                setTimeout(function () {
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

                        var list = formChangePassword.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                }
            },
        watch: {
            currentPassword(value) {

                this.errors = this.errors.filter(a => a.id !== "currentPassword");

                if (value != null)
                    this.status = "";

                currentPassword.setCustomValidity("");
            },
            password(value) {
                this.errors = this.errors.filter(a => a.id !== "password");

                if (value != null)
                    this.status = "";

                password.setCustomValidity("");
            },
            passwordConfirm(value) {

                this.errors = this.errors.filter(a => a.id !== "passwordConfirm");

                if (value != null)
                    this.status = "";

                passwordConfirm.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')