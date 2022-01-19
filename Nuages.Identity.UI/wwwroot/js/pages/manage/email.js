var App =
    {
        data() {
            return {
                email: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            email.focus();
        },
        methods:
            {
                doChangeEmail: function () {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";
                    
                    fetch("/api/manage/sendEmailChange", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                email: e
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            
                            
                            if (res.success) {
                                self.status = "done";
                            } else
                                self.status = "";
                                self.errors.push({message: res.errors[0]});
                        });

                },
                changeEmail: function () {

                    this.errors = [];
                    formChangeEmail.classList.remove("was-validated");

                    email.setCustomValidity("");
                    
                    var res = formChangeEmail.checkValidity();
                    if (res) {
                       this.doChangeEmail();
                    } else {
                        formChangeEmail.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formChangeEmail.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            },
        watch: {
            email(value) {
                this.errors = [];
                this.status = "";
                email.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')