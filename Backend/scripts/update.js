const fs = require('fs');

const file = 'Frontend/src/views/HomeSite/Teams/TeamKudos.vue';
let content = fs.readFileSync(file, 'utf-8');

const newScript = `<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePeopleStore } from '@/store/usePeopleStore'
import { useTeamStore } from '@/store/useTeamStore'
import { useHomeProjectStore } from '@/store/useHomeProjectStore'
import { useSiteStore } from '@/store/useSiteStore'
import UserAvatar from '@/components/common/UserAvatar.vue'

const peopleStore = usePeopleStore()
const teamStore = useTeamStore()
const projectStore = useHomeProjectStore()
const siteStore = useSiteStore()

onMounted(async () => {
  if (siteStore.sites.length === 0) {
    await siteStore.fetchSites()
  }
  if (!peopleStore.users || peopleStore.users.length === 0) {
    await peopleStore.fetchPeople()
  }
  if (!teamStore.allTeams || teamStore.allTeams.length === 0) {
    await teamStore.fetchAllTeams()
  }
  await projectStore.fetchProjects()
  await teamStore.fetchRecentKudos()
  
  if (!kudosTargetId.value) {
    const firstTeam = teamStore.allTeams[0]
    const firstPerson = peopleStore.users[0]
    if (firstTeam) {
      selectKudosTarget('team', firstTeam)
    } else if (firstPerson) {
      selectKudosTarget('user', firstPerson)
    }
  }
})

const sanitizeHtml = (value) => {
    return value;
}

// Kudos Variables
const isGiveKudosOpen = ref(false)
const kudosText = ref('')
const isKudosLinkDropdownOpen = ref(false)
const isKudosTargetDropdownOpen = ref(false)
const isKudosEmojiDropdownOpen = ref(false)
const kudosLinkSearch = ref('')
const kudosLinkDisplay = ref('')
const kudosLinkTab = ref('Home')
const kudosEmojiSearch = ref('')

const isKudosGraphicDropdownOpen = ref(false)
const kudosGraphics = [
  { bg: '#0052CC', c1: '#FF8F73', c2: '#FF5630', icon1: 'fa-solid fa-fish-fins', icon2: 'fa-solid fa-box-open' },
  { bg: '#00875A', c1: '#FFC400', c2: '#FFAB00', icon1: 'fa-solid fa-star', icon2: 'fa-solid fa-trophy' },
  { bg: '#FF5630', c1: '#00B8D9', c2: '#008DA6', icon1: 'fa-solid fa-bolt', icon2: 'fa-solid fa-medal' },
  { bg: '#6554C0', c1: '#FF7452', c2: '#FF5630', icon1: 'fa-solid fa-heart', icon2: 'fa-solid fa-gem' },
  { bg: '#36B37E', c1: '#0052CC', c2: '#FFC400', icon1: 'fa-solid fa-thumbs-up', icon2: 'fa-solid fa-check-circle' },
  { bg: '#FFAB00', c1: '#6554C0', c2: '#FF5630', icon1: 'fa-solid fa-crown', icon2: 'fa-solid fa-award' },
  { bg: '#00B8D9', c1: '#36B37E', c2: '#FF8F73', icon1: 'fa-solid fa-lightbulb', icon2: 'fa-solid fa-rocket' },
  { bg: '#172B4D', c1: '#00B8D9', c2: '#0052CC', icon1: 'fa-solid fa-handshake', icon2: 'fa-solid fa-hand-holding-heart' }
]
const selectedKudosGraphic = ref(kudosGraphics[0])

const selectKudosGraphic = (g) => {
  selectedKudosGraphic.value = g
  isKudosGraphicDropdownOpen.value = false
}

const allEmojis = ['😀','😃','😄','😁','😆','😅','😂','🤣','😊','😇','🙂','🙃','😉','😌','😍','🥰','😘','😗','😙','😚','😋','😛','😝','😜','🤪','🤨','🧐','🤓','😎','🤩','🥳','😏','😒','😞','😔','😟','😕','🙁','☹️','😣','😖','😫','😩','🥺','😢','😭','😤','😠','😡','🤬','🤯','😳','🥵','🥶','😱','😨','😰','😥','😓','🤗','🤔','🤭','🤫','🤥','😶','😐','😑','😬','🙄','😯','😦','😧','😮','😲','🥱','😴','🤤','😪','😵','🤐','🥴','🤢','🤮','🤧','😷','🤒','🤕','🤑','🤠','😈','👿','👹','👺','🤡','💩','👻','💀','☠️','👽','👾','🤖','🎃','😺','😸','😹','😻','😼','😽','🙀','😿','😾','🙈','🙉','🙊','💥','💫','💦','💨','🐵','🐒','🦍','🦧','🐶','🐕','🦮','🐕‍🦺','🐩','🐺','🦊','🦝','🐱','🐈','🦁','🐯','🐅','🐆','🐴','🐎','🦄','🦓','🦌','🐮','🐂','🐃','🐄','🐷','🐖','🐗','🐽','🐏','🐑','🐐','🐪','🐫','🦙','🦒','🐘','🦏','🦛','🐭','🐁','🐀','🐹','🐰','🐇','🐿️','🦔','🦇','🐻','🐨','🐼','🦥','🦦','🦨','🦘','🦡','🐾','🦃','🐔','🐓','🐣','🐤','🐥','🐦','🐧','🕊️','🦅','🦆','🦢','🦉','🦩','🦚','🦜','🐸','🐊','🐢','🦎','🐍','🐲','🐉','🦕','🦖','🐳','🐋','🐬','🐟','🐠','🐡','🦈','🐙','🐚','🐌','🦋','🐛','🐜','🐝','🐞','🦗','🕷️','🕸️','🦂','🦟','🦠','💐','🌸','💮','🏵️','🌹','🥀','🌺','🌻','🌼','🌷','🌱','🌲','🌳','🌴','🌵','🌾','🌿','☘️','🍀','🍁','🍂','🍃','🍇','🍈','🍉','🍊','🍋','🍌','🍍','🥭','🍎','🍏','🍐','🍑','🍒','🍓','🥝','🍅','🥥','🥑','🍆','🥔','🥕','🌽','🌶️','🥒','🥬','🥦','🧄','🧅','🍄','🥜','🌰','🍞','🥐','🥖','🥨','🥯','🥞','🧇','🧀','🍖','🍗','🥩','🥓','🍔','🍟','🍕','🌭','🥪','🌮','🌯','🥙','🧆','🥚','🍳','🥘','🍲','🥣','🥗','🍿','🧈','🧂','🥫','🍱','🍘','🍙','🍚','🍛','🍜','🍝','🍠','🍢','🍣','🍤','🍥','🥮','🍡','🥟','🥠','🥡','🦀','🦞','🦐','🦑','🦪','🍦','🍧','🍨','🍩','🍪','🎂','🍰','🧁','🥧','🍫','🍬','🍭','🍮','🍯','🍼','🥛','☕','🍵','🍶','🍾','🍷','🍸','🍹','🍺','🍻','🥂','🥃','🥤','🧃','🧉','🧊','🥢','🍽️','🍴','🥄','🔪','🏺','🎉','👍','🚀','❤️','🔥','👏','🙌','💯','💪','✨','🌟']
const filteredKudosEmojis = computed(() => {
  if (!kudosEmojiSearch.value) return allEmojis
  return allEmojis.slice(0, 10)
})

const kudosEditorRef = ref(null)

const kudosTargetType = ref('team')
const kudosTargetName = ref('Đội ngũ của bạn')
const kudosTargetAvatar = ref('Đ')
const kudosTargetId = ref(null)
const kudosTargetData = ref(null)

const kudosTargetUsers = computed(() => {
  return peopleStore.users || []
})

const siteProjects = computed(() => projectStore.projects || []) 
const projects = computed(() => projectStore.projects || []) 

const selectKudosTarget = (type, item) => {
  kudosTargetType.value = type
  kudosTargetName.value = item.name || item.fullName || item.email
  kudosTargetAvatar.value = item.initials || (kudosTargetName.value ? kudosTargetName.value.substring(0, 2).toUpperCase() : 'T')
  kudosTargetData.value = item
  kudosTargetId.value = item.id
  isKudosTargetDropdownOpen.value = false
}

const insertEmoji = (emoji) => {
  kudosText.value = (kudosText.value || '') + emoji
  if (kudosEditorRef.value) kudosEditorRef.value.innerHTML = kudosText.value
  isKudosEmojiDropdownOpen.value = false
}

const selectKudosLink = (item) => {
  kudosLinkSearch.value = item.name || item.title
  kudosLinkDisplay.value = item.name || item.title
}

const insertKudosLink = () => {
  const text = kudosLinkDisplay.value || kudosLinkSearch.value
  if (text) {
    const linkHtml = \`<a href="/home/projects" style="color: #0052CC; text-decoration: none; font-weight: 500;" contenteditable="false">\${text}</a>&nbsp;\`
    kudosText.value = (kudosText.value || '') + ' ' + linkHtml
    if (kudosEditorRef.value) kudosEditorRef.value.innerHTML = kudosText.value
    isKudosLinkDropdownOpen.value = false
  }
}

const submitKudos = async () => {
  isGiveKudosOpen.value = false
  
  let finalMessage = kudosEditorRef.value?.innerHTML || kudosText.value

  const payload = {
    message: finalMessage,
    icon: selectedKudosGraphic.value?.icon2 || 'fa-solid fa-box-open'
  }
  
  if (kudosTargetType.value === 'team') {
    payload.departmentId = kudosTargetData.value?.id || kudosTargetId.value
  } else {
    payload.receiverId = kudosTargetData.value?.id || kudosTargetId.value
  }

  try {
    await teamStore.sendKudos(payload)
    kudosText.value = ''
    if (kudosEditorRef.value) kudosEditorRef.value.innerHTML = ''
  } catch (error) {
    console.error('Failed to submit kudos', error)
  }
}
</script>`;

content = content.replace(/<script setup>[\s\S]*?<\/script>/, newScript);
fs.writeFileSync(file, content);
console.log('Update Complete.');
