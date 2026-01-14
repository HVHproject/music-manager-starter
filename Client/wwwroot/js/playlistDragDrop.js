window.playlistDragDrop = {
    instance: null,

    initialize: function (elementId, dotNetHelper) {
        const el = document.getElementById(elementId);
        if (!el) return;

        if (this.instance) {
            this.instance.destroy();
        }

        this.instance = Sortable.create(el, {
            animation: 150,
            handle: '.drag-handle',
            ghostClass: 'opacity-50',
            dragClass: 'border-indigo-500',
            onEnd: function (evt) {
                dotNetHelper.invokeMethodAsync('OnSongReordered', evt.oldIndex, evt.newIndex);
            }
        });
    },

    destroy: function () {
        if (this.instance) {
            this.instance.destroy();
            this.instance = null;
        }
    },

    setEnabled: function (enabled) {
        if (this.instance) {
            this.instance.option('disabled', !enabled);
        }
    }
};