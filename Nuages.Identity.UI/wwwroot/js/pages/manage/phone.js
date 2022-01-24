var App =
    {
        data() {
            return {
                phone: "",
                dialCode: "+1",
                code: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            phone.focus();
        },
        methods:
            {
                doSendPhoneChangeMessage: function () {
                    var self = this;
                    var p = self.dialCode + self.phone;
            
                    this.status = "sending";
                    
                    fetch("/api/manage/sendPhoneChangeMessage", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            phoneNumber: p
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            
                            if (res.success) {
                                self.status = "sent";
                                setTimeout(function()
                                {
                                    code.focus();
                                }, 0);
                                
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                },
                sendPhoneChangeMessage: function () {

                    this.errors = [];
                    formAddPhone.classList.remove("was-validated");

                    phone.setCustomValidity("");
                    
                    var res = formAddPhone.checkValidity();
                    if (res) {
                       this.doSendPhoneChangeMessage();
                    } else {
                        formAddPhone.classList.add("was-validated");

                        if (!phone.validity.valid) {
                            phone.setCustomValidity(phoneRequiredMessage);
                        }

                        var list = formAddPhone.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });
                    }
                },
                doAddPhone: function () {
                    var self = this;
                    var p = self.dialCode + self.phone;
                    var c = self.code;
                    
                    this.status = "sending";

                    fetch("/api/manage/changePhoneNumber", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            phoneNumber: p,
                            token: c
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                //self.status = "done";
                                window.location = "/account/manage/twoFactorAuthentication";
                            } else
                                self.status = "sent";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                },
                addPhone: function () {

                    this.errors = [];
                    formAddPhone.classList.remove("was-validated");

                    code.setCustomValidity("");
                    
                    var res = formAddPhone.checkValidity();
                    if (res) {
                        this.doAddPhone();
                    } else {
                        formAddPhone.classList.add("was-validated");

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formAddPhone.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });
                    }
                }
            },
        watch: {
            phone(value) {
                this.errors = [];
                this.status = "";
                phone.setCustomValidity("");
            },
            code(value) {
                this.errors = [];
                //this.status = "";
                code.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')