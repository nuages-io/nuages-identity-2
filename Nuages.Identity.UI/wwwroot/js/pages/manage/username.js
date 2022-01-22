var App =
    {
        data() {
            return {
                username: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            username.focus();
        },
        methods:
            {
                doChangeUsername: function () {
                    var self = this;
                    var u = self.username;

                    this.status = "sending";
                    
                    fetch("/api/manage/changeUsername", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            newUsername: u
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
                        });

                },
                changeUsername: function () {

                    this.errors = [];
                    formChangeUsername.classList.remove("was-validated");

                    username.setCustomValidity("");
                    
                    var res = formChangeUsername.checkValidity();
                    if (res) {
                       this.doChangeUsername();
                    } else {
                        formChangeUsername.classList.add("was-validated");

                        if (!username.validity.valid) {
                            username.setCustomValidity(usernameRequiredMessage);
                        }

                        var list = formChangeUsername.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });
                    }
                }
            },
        watch: {
            username(value) {
                this.errors = [];
                this.status = "";
                username.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')