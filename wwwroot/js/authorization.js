function togglePassword(inputId, eyeId) {
    const passwordInput = document.getElementById(inputId);
    const eyeIcon = document.getElementById(eyeId);

    eyeIcon.addEventListener("click", () => {
        const isPassword = passwordInput.type === "password";
        passwordInput.type = isPassword ? "text" : "password";
        eyeIcon.src = isPassword ? "/images/eye.svg" : "/images/eye_slash.svg";
        eyeIcon.alt = isPassword ? "o" : "\u2205";
    });
}

togglePassword("input-password", "eye-password");
togglePassword("input-repeat-password", "eye-repeat-password");