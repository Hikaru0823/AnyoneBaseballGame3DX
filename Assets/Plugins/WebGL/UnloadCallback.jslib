mergeInto(LibraryManager.library, {
  SetupOnUnloadCallback: function () {
    window.addEventListener("beforeunload", function () {
      SendMessage("TitleUI", "OnPageUnload");
    });
  }
});