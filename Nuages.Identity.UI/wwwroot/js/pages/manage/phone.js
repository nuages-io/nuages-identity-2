var App =
    {
        data() {
            return {
                phone: "",
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
                    var p = self.phone;

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
                    formChangePhone.classList.remove("was-validated");

                    phone.setCustomValidity("");
                    
                    var res = formChangePhone.checkValidity();
                    if (res) {
                       this.doSendPhoneChangeMessage();
                    } else {
                        formChangePhone.classList.add("was-validated");

                        if (!phone.validity.valid) {
                            phone.setCustomValidity(phoneRequiredMessage);
                        }

                        var list = formChangePhone.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                },
                doChangePhone: function () {
                    var self = this;
                    var p = self.phone;
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
                                self.status = "done";
                            } else
                                self.status = "sent";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });

                        });

                },
                changePhone: function () {

                    this.errors = [];
                    formChangePhone.classList.remove("was-validated");

                    phone.setCustomValidity("");
                    code.setCustomValidity("");
                    
                    var res = formChangePhone.checkValidity();
                    if (res) {
                        this.doChangePhone();
                    } else {
                        formChangePhone.classList.add("was-validated");

                        if (!phone.validity.valid) {
                            phone.setCustomValidity(phoneRequiredMessage);
                        }

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formChangePhone.querySelectorAll(":invalid");

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