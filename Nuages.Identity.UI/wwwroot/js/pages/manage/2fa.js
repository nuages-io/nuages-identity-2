var App =
    {
        data() {
            return {
                
                showDeactivate: false,
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