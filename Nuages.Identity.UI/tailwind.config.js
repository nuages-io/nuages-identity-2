module.exports = {
    purge: {
        enabled: true,
        content: [
            './Pages/**/*.cshtml',
            '.wwwroot/js/pages/**/*.js'
        ]
    },
    darkMode: 'class', // or 'media' or 'class'
    theme: {
        extend: {},
    },
    variants: {
        extend: {
            backgroundColor: ['disabled'],
            borderStyle: ['focus']
        }
    },
    plugins: [
        require('@tailwindcss/forms'),
        require('@tailwindcss/typography'),
    ]
}
