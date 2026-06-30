import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export const useLayoutStore = defineStore('layout', () => {
  // State
  const sidebarOpen = ref<boolean>(false)
  const sidebarRail = ref<boolean>(false)
  const showNotifications = ref<boolean>(false)
  const sidebarWidth = ref<number>(280)

  // Computed
  const isMobileSidebar = computed(() => {
    // In a real app, this would check window.matchMedia
    return false
  })

  const sidebarCollapsed = computed(() => {
    return sidebarRail.value || !sidebarOpen.value
  })

  // Actions
  /**
   * Toggle sidebar visibility
   */
  const toggleSidebar = (): void => {
    sidebarOpen.value = !sidebarOpen.value
  }

  /**
   * Open sidebar
   */
  const openSidebar = (): void => {
    sidebarOpen.value = true
  }

  /**
   * Close sidebar
   */
  const closeSidebar = (): void => {
    sidebarOpen.value = false
  }

  /**
   * Toggle sidebar rail mode (icon-only)
   */
  const toggleSidebarRail = (): void => {
    sidebarRail.value = !sidebarRail.value
  }

  /**
   * Toggle notifications panel
   */
  const toggleNotifications = (): void => {
    showNotifications.value = !showNotifications.value
  }

  /**
   * Set sidebar width
   */
  const setSidebarWidth = (width: number): void => {
    sidebarWidth.value = Math.max(200, Math.min(400, width))
  }

  /**
   * Close all panels (sidebar, notifications, etc)
   */
  const closeAllPanels = (): void => {
    sidebarOpen.value = false
    showNotifications.value = false
  }

  // Return public API
  return {
    // State
    sidebarOpen,
    sidebarRail,
    showNotifications,
    sidebarWidth,

    // Computed
    isMobileSidebar,
    sidebarCollapsed,

    // Actions
    toggleSidebar,
    openSidebar,
    closeSidebar,
    toggleSidebarRail,
    toggleNotifications,
    setSidebarWidth,
    closeAllPanels,
  }
})
