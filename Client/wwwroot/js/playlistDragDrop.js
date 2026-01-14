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

            onStart: function (evt) {
                const parent = evt.from;
                evt.from._originalOrder = Array.from(parent.children);
            },

            onEnd: function (evt) {
                const oldIdx = evt.oldIndex;
                const newIdx = evt.newIndex;

                if (oldIdx !== newIdx && evt.from._originalOrder) {
                    const parent = evt.from;

                    while (parent.firstChild) {
                        parent.removeChild(parent.firstChild);
                    }

                    evt.from._originalOrder.forEach(child => {
                        parent.appendChild(child);
                    });

                    delete evt.from._originalOrder;
                }

                dotNetHelper.invokeMethodAsync('OnSongReordered', oldIdx, newIdx);
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