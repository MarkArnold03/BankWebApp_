// Theme toggle: cycles between light and dark, remembered per browser.
(function () {
    var root = document.documentElement;

    function currentTheme() {
        var set = root.getAttribute("data-theme");
        if (set) return set;
        return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    document.querySelectorAll("[data-theme-toggle]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var next = currentTheme() === "dark" ? "light" : "dark";
            root.setAttribute("data-theme", next);
            try { localStorage.setItem("theme", next); } catch (e) { }
        });
    });
})();

// Close the account menu when clicking elsewhere.
document.addEventListener("click", function (e) {
    document.querySelectorAll("details.usermenu[open]").forEach(function (d) {
        if (!d.contains(e.target)) d.removeAttribute("open");
    });
});
