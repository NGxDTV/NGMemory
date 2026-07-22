(function () {
  const root = document.documentElement;
  const langButtons = Array.from(document.querySelectorAll("[data-set-lang]"));
  const searchInput = document.getElementById("searchInput");
  const searchResults = document.getElementById("searchResults");
  const tocLinks = Array.from(document.querySelectorAll(".toc a"));

  function setLanguage(lang) {
    root.dataset.lang = lang;
    root.lang = lang;
    localStorage.setItem("ngmemory-doc-lang", lang);

    langButtons.forEach((button) => {
      button.classList.toggle("active", button.dataset.setLang === lang);
    });

    if (searchInput) {
      searchInput.placeholder = lang === "de"
        ? "API, Overlay, Scanner..."
        : "API, overlay, scanner...";
    }

    renderSearch();
  }

  function normalize(value) {
    return (value || "").toLowerCase().replace(/\s+/g, " ").trim();
  }

  function isInPagesFolder() {
    return /\/pages\/|\\pages\\/.test(window.location.pathname);
  }

  function toRelativeUrl(url) {
    return isInPagesFolder() ? "../" + url : url;
  }

  function renderSearch() {
    if (!searchInput || !searchResults || !window.NG_MEMORY_SEARCH_INDEX) {
      return;
    }

    const query = normalize(searchInput.value);
    searchResults.innerHTML = "";

    if (!query) {
      searchResults.classList.remove("visible");
      return;
    }

    const lang = root.dataset.lang || "de";
    const matches = window.NG_MEMORY_SEARCH_INDEX
      .map((item) => {
        const haystack = normalize([
          item.title,
          item.de,
          item.en,
          item.keywords
        ].join(" "));

        return haystack.includes(query) ? item : null;
      })
      .filter(Boolean)
      .slice(0, 8);

    searchResults.classList.add("visible");

    if (matches.length === 0) {
      const empty = document.createElement("div");
      empty.className = "search-empty";
      empty.textContent = lang === "de" ? "Keine Treffer gefunden." : "No results found.";
      searchResults.appendChild(empty);
      return;
    }

    matches.forEach((item) => {
      const link = document.createElement("a");
      link.href = toRelativeUrl(item.url);

      const title = document.createElement("b");
      title.textContent = item.title;

      const text = document.createElement("span");
      text.textContent = lang === "de" ? item.de : item.en;

      link.appendChild(title);
      link.appendChild(text);
      searchResults.appendChild(link);
    });
  }

  function updateActiveToc() {
    const current = window.location.pathname.replace(/\\/g, "/").split("/").pop() || "index.html";

    tocLinks.forEach((link) => {
      const href = link.getAttribute("href") || "";
      const target = href.split("#")[0].split("/").pop() || "index.html";
      link.classList.toggle("active", target === current);
    });
  }

  function escapeHtml(value) {
    return value
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");
  }

  function highlightCSharp(code) {
    let html = escapeHtml(code);

    html = html.replace(/(\/\/.*)$/gm, '<span class="tok-comment">$1</span>');
    html = html.replace(/("(?:\\.|[^"\\])*")/g, '<span class="tok-string">$1</span>');
    html = html.replace(/\b(0x[0-9a-fA-F]+|\d+(?:\.\d+)?f?)\b/g, '<span class="tok-number">$1</span>');
    html = html.replace(/\b(using|namespace|class|public|private|protected|internal|static|void|var|new|return|if|else|foreach|in|true|false|null|bool|int|string|short|long|float|double|byte|object|event|override|readonly|sealed|yield|base)\b/g, '<span class="tok-keyword">$1</span>');
    html = html.replace(/\b(IntPtr|Rectangle|Point|Bitmap|Color|Timer|Form|FormClosingEventArgs|Console|Process|Dictionary|IEnumerable|Encoding|CaptureBlackoutProtector|ScreenshotBlurProtector|CaptureMaskControl|CaptureMaskViewModel|ProtectedAreaManager|EasyWindow|EasyKeyboard|EasyPressKey|EasyMouse|EasyWait|EasyScreen|EasyScreenAnalysis|EasySysListView32|EasyFormHelper|EasyTextBox|EasyButton|EasyCheckBox|EasyComboBox|EasyElementFinder|EasyMemory|EasyDebugHook|WindowDisplayAffinity|OverlayManager|OverlayPosition|TargetWindowType|OverlayStyleHelper|WindowStyleHelper|GuiInteropHandler|InputHelper|MenuStripHelper|VAMemory|Scanner|Module|DebugHook|Enums|MouseButton)\b/g, '<span class="tok-type">$1</span>');

    return html;
  }

  function prepareCodeBlocks() {
    document.querySelectorAll("pre code").forEach((code) => {
      const raw = code.textContent;
      code.dataset.raw = raw;

      if (code.classList.contains("language-csharp")) {
        code.innerHTML = highlightCSharp(raw);
      } else {
        code.textContent = raw;
      }

      const card = code.closest(".code-card");
      if (!card || card.querySelector(".copy-button")) {
        return;
      }

      const button = document.createElement("button");
      button.type = "button";
      button.className = "copy-button";
      button.textContent = "Copy";
      button.addEventListener("click", async () => {
        try {
          await navigator.clipboard.writeText(raw);
          button.textContent = "Copied";
        } catch {
          button.textContent = "Nope";
        }

        window.setTimeout(() => {
          button.textContent = "Copy";
        }, 1400);
      });

      card.appendChild(button);
    });
  }

  langButtons.forEach((button) => {
    button.addEventListener("click", () => setLanguage(button.dataset.setLang));
  });

  if (searchInput) {
    searchInput.addEventListener("input", renderSearch);
  }

  prepareCodeBlocks();
  setLanguage(localStorage.getItem("ngmemory-doc-lang") || "de");
  updateActiveToc();
})();
