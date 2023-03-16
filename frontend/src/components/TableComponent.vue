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
            <input v-if="type == 'text' || type == 'checkbox'" :type="type" v-model="row[title.toLowerCase()]" />
            <select v-else-if="type == 'select activity'" v-model="row[title.toLowerCase()]">
              <option disabled value="">Select an activity</option>
              <option v-for="(activityTitle, i) in activityTitles" :key="i"> {{ activityTitle }} </option>
            </select>
            <select v-else-if="type == 'select relation'" v-model="row[title.toLowerCase()]">
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
    }
  }
}
</script>

<style scoped lang="scss" />
