import { VueRenderer } from '@tiptap/vue-3'
import tippy from 'tippy.js'
import MentionList from './MentionList.vue'
import { usePeopleStore } from '@/store/usePeopleStore'

export default {
  items: ({ query }) => {
    const peopleStore = usePeopleStore()
    const users = peopleStore.users || []
    return users.filter(item => {
      const name = item.fullName || ''
      const email = item.email || ''
      return name.toLowerCase().includes(query.toLowerCase()) || email.toLowerCase().includes(query.toLowerCase())
    }).slice(0, 5)
  },

  render: () => {
    let component
    let popup

    return {
      onStart: props => {
        component = new VueRenderer(MentionList, {
          props,
          editor: props.editor,
        })

        if (!props.clientRect) {
          return
        }

        popup = tippy('body', {
          getReferenceClientRect: props.clientRect,
          appendTo: () => document.body,
          content: component.element,
          showOnCreate: true,
          interactive: true,
          trigger: 'manual',
          placement: 'bottom-start',
        })
      },

      onUpdate(props) {
        component.updateProps(props)

        if (!props.clientRect) {
          return
        }

        popup[0].setProps({
          getReferenceClientRect: props.clientRect,
        })
      },

      onKeyDown(props) {
        if (props.event.key === 'Escape') {
          popup[0].hide()
          return true
        }

        return component.ref?.onKeyDown(props)
      },

      onExit() {
        if (popup && popup[0]) {
          popup[0].destroy()
        }
        if (component) {
          component.destroy()
        }
      },
    }
  },
}
