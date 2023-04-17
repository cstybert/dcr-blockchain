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
          <td v-for="({mapping, type}, j) in headers" :key="j">
            <!-- Text fields (e.g. Title) -->
            <input v-if="type == 'text'"
              type="text"
              :disabled="disabled == true ? disabled : executeMode"
              v-model="row[mapping]" />

            <!-- Boolean fields (e.g. Pending) -->
            <input v-else-if="type == 'checkbox'"
              type="checkbox"
              :disabled="disabled == true ? disabled : executeMode"
              v-model="row[mapping]" />

            <!-- Button fields (e.g. Execute) -->
            <button v-else-if="type == 'button'"
              :disabled="disabled == true ? disabled : (!executeMode || (executeMode && !row['enabled']))"
              @click="executeMode ? executeActivity(row) : null"> Execute </button>
            
            <!-- Select activity fields (e.g. Source) -->
            <select v-else-if="type == 'select activity'" :disabled="disabled == true ? disabled : executeMode" v-model="row[mapping]">
                <option disabled value="">Select an activity</option>
                <option v-for="(activityTitle, i) in activityTitles" :key="i"> {{ activityTitle }} </option>
            </select>

            <!-- Select relation field (e.g. Type) -->
            <select v-else-if="type == 'select relation'" :disabled="disabled == true ? disabled : executeMode" v-model="row[mapping]">
              <option disabled value="">Select a relation type</option>
              <option :value="id" v-for="({id}, i) in relationTypes" :key="i"> {{ id }} </option>
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
    },
    disabled: {
      type: Boolean,
      default: false
    }
  },

  methods: {
    async executeActivity(activity) {
      activity['executed'] = true;
      this.$emit('executeActivity', activity['title']);
    }
  }
}
</script>

<style scoped lang="scss" />
