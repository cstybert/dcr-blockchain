<template>
  <div class="table-component">
    <table class="table">
      <thead>
        <tr>
          <th v-for="({title}, i) in headers" :key="i">
            {{ title }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, i) in data" :key="i">
          <td v-for="({title, type}, i) in headers" :key="i">
            <!-- Text fields (e.g. Title) -->
            <input v-if="type == 'text'"
              type="text"
              :disabled="executeMode"
              v-model="row[title.toLowerCase()]" />

            <!-- Boolean fields (e.g. Pending) -->
            <input v-else-if="type == 'checkbox'"
              type="checkbox"
              :disabled="(executeMode && title != 'Executed') ||
                         (executeMode && title == 'Executed' && row[title.toLowerCase()])"
              @click="executeMode ? executeActivity(row['title']) : null"
              v-model="row[title.toLowerCase()]" />
            
            <!-- Select activity fields (e.g. Source) -->
            <select v-else-if="type == 'select activity'" :disabled="executeMode" v-model="row[title.toLowerCase()]">
                <option disabled value="">Select an activity</option>
                <option v-for="(activityTitle, i) in activityTitles" :key="i"> {{ activityTitle }} </option>
            </select>

            <!-- Select relation field (e.g. Type) -->
            <select v-else-if="type == 'select relation'" :disabled="executeMode" v-model="row[title.toLowerCase()]">
              <option disabled value="">Select a relation type</option>
              <option v-for="({id}, i) in relationTypes" :key="i"> {{ id }} </option>
            </select>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import "./TableComponent.scss";

export default {
  name: 'TableComponent',
  props: {
    headers: {
      type: Array,
      required: true
    },
    data: {
      type: Array,
      required: true
    },
    activityTitles: {
      type: Array,
      required: false
    },
    relationTypes: {
      type: Array,
      required: false
    },
    executeMode: {
      type: Boolean,
      required: false,
      default: false
    }
  },

  methods: {
    async executeActivity(title) {
      this.$emit('executeActivity', title);
    }
  }
}
</script>

<style scoped lang="scss" />
