define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
    Sitecore.Commands.ShowDictionaryItems =
    {
        canExecute: function (context) {
            var dictionaryItems = window.parent.document.querySelectorAll(".dictionary-item");
            var visibility = context.button.get("isChecked") === "1" ? "visible" : "hidden";
            for (var i = 0; i < dictionaryItems.length; i++) {
                dictionaryItems[i].style.visibility = visibility;
            }
            return true;
        },
        execute: function (context) {
            ExperienceEditor.PipelinesUtil.generateRequestProcessor("ExperienceEditor.ToggleRegistryKey.Toggle", function (response) {
                response.context.button.set("isChecked", response.responseValue.value ? "1" : "0");
                var dictionaryItems = window.parent.document.querySelectorAll(".dictionary-item");
                var visibility = response.responseValue.value ? "visible" : "hidden";
                for (var i = 0; i < dictionaryItems.length; i++) {
                    dictionaryItems[i].style.visibility = visibility;
                }
            }, { value: context.button.get("registryKey") }).execute(context);
        }
    };
});