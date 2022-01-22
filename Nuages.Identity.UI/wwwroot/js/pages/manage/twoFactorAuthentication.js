var App =
    {
        data() {
            return {                
                showDeactivate: false,
                showRemovePhone: false,
                showCodes: false,
                errors: [],
                status: ""
            }
        },
        mounted() {
            
        },
        methods:
            {
               
                forgetBrowser()
                {
                    //Call API
                },
                disable2Fa()
                {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/disable2FA", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                },
                removePhone()
                {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/removePhone", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                }               
                
            }
    };


Vue.createApp(App).mount('#app')