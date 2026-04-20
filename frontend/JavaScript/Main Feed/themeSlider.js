(function () {
  // Catppuccin Mocha
  const dark = {
    bg: "#1e1e2e", fg: "#cdd6f4", muted: "#6c7086", card: "#181825", border: "#313244",
    accent: "#cba6f7", accent2: "#89b4fa", overlay: "#313244",
  };
  // Catppuccin Latte
  const light = {
    bg: "#eff1f5", fg: "#4c4f69", muted: "#9ca0b0", card: "#e6e9ef", border: "#ccd0da",
    accent: "#8839ef", accent2: "#1e66f5", overlay: "#dce0e8",
  };

  function applyTheme(isDark) {
    const t = isDark ? dark : light;
    const root = document.documentElement.style;
    root.setProperty("--bg",      t.bg);
    root.setProperty("--fg",      t.fg);
    root.setProperty("--muted",   t.muted);
    root.setProperty("--card",    t.card);
    root.setProperty("--border",  t.border);
    root.setProperty("--accent",  t.accent);
    root.setProperty("--accent2", t.accent2);
    root.setProperty("--overlay", t.overlay);
  }

  function updateButton(btn, isDark) {
    btn.textContent = isDark ? "☀️ Light mode" : "🌙 Dark mode";
  }

  function init() {
    const btn = document.getElementById("theme-toggle");
    if (!btn) return;

    const saved = localStorage.getItem("theme");
    let isDark = saved !== null ? saved === "dark" : true; // default: dark

    applyTheme(isDark);
    updateButton(btn, isDark);

    // Easter egg: 5 rapid clicks (within 3 s) navigates away
    let clickCount = 0;
    let lastClickTs = 0;
    const EASTER_WINDOW_MS = 3000;
    const EASTER_CLICKS    = 5;

    btn.addEventListener("click", () => {
      const now = Date.now();
      if (now - lastClickTs > EASTER_WINDOW_MS) clickCount = 0;
      clickCount++;
      lastClickTs = now;

      if (clickCount >= EASTER_CLICKS) {
        clickCount = 0;
        try { window.location.href = "https://csabi.zip"; } catch (_) {}
        return;
      }

      // Normal toggle
      isDark = !isDark;
      applyTheme(isDark);
      updateButton(btn, isDark);
      try { localStorage.setItem("theme", isDark ? "dark" : "light"); } catch (_) {}
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
