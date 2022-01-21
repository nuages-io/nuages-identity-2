var App =
    {
        data() {
            return {
                
                showDeactivate: false,
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

                            //self.errors.push({message: res.errors[0]});
                        });
                },
                addFallbackPhone()
                {
                    //Call API
                },
                removeFallbackPhone()
                {
                    //Call API
                }               
                
            }
    };


Vue.createApp(App).mount('#app')