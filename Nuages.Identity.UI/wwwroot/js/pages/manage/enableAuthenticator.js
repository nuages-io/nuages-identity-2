var App =
    {
        data() {
            return {
                code: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            code.focus();
        },
        methods:
            {
                doVerifyCode: function () {
                    var self = this;
                    var c = self.code;

                    this.status = "sending";

                    fetch("/api/manage/verify2FACode", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                code: c
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                self.status = "done";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });

                            //self.errors.push({message: res.errors[0]});
                        });

                },
                verifyCode: function () {

                    this.errors = [];
                    formCode.classList.remove("was-validated");

                    code.setCustomValidity("");

                    var res = formCode.checkValidity();
                    if (res) {
                        this.doVerifyCode();
                    } else {
                        formCode.classList.add("was-validated");

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formCode.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            },
        watch: {
            code(value) {
                this.errors = [];
                this.status = "";
                code.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')