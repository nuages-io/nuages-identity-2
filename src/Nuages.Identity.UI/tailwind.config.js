module.exports = {
    content: [
        './Pages/**/*.cshtml',
        '.wwwroot/js/pages/**/*.js'
    ],
    darkMode: 'media', // or 'media' or 'class'
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
