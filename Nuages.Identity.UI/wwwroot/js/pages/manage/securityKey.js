var App =
    {
        data() {
            return {
                nickname: "",
                username: userName,
                errors: [],
                status: ""
            }
        },
        mounted() {
            nickname.focus();
        },
        methods:
            {
                 addSecurityKey: function () {

                    this.errors = [];
                    
                    formAddSecurityKey.classList.remove("was-validated");

                    nickname.setCustomValidity("");

                    var res = formAddSecurityKey.checkValidity();
                    if (res) {
                        
                        var self = this;

                        self.status = "adding";

                        var n = self.nickname;
                        var u = self.username;
                        
                        handleAddSecurityKey({
                            UserName: u,
                            DisplayName: n,
                            AttestationType: "none",
                            UserVerification: "preferred",
                            AuthType: null,
                            RequireResidentKey: false
                        }, this.onCallback);
                    } else {
                        formAddSecurityKey.classList.add("was-validated");

                        if (!nickname.validity.valid) {
                            nickname.setCustomValidity(nickNameRequiredMessage);
                        }

                        var list = formAddSecurityKey.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                },
                onCallback : function(event, data)
                {
                    switch(event)
                    {
                        case "success":
                        {
                            this.status = "done";
                            setTimeout(function()
                            {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            });
                            break;
                        }
                        case "registering":
                        {
                            this.status = "registering";
                            break;
                        }
                        case "error":
                        {
                            this.status = "";
                            this.errors.push({message: data});
                            break;
                        }
                    }
                }
            },
        watch: {
            nickname(value) {
                this.errors = [];
                nickname.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')