<template>
  <div class="link-table-component">
    <table class="table">
      <thead>
          <th v-for="({title}, i) in headers" :key="i">
            {{ title }}
          </th>
      </thead>
      <tbody>
        <tr v-for="(graph, i) in data" :key="i" @click="selectGraph(graph)" :class="{'highlight': (graph['id'] == selectedGraph)}">
          <td v-for="({mapping}, j) in headers" :key="j">
            {{ graph[mapping] }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import "./LinkTableComponent.scss";

export default {
  name: 'LinkTableComponent',
  props: {
    headers: {
      type: Array,
      required: true
    },
    data: {
      type: Array,
      required: true
    },
    selectedGraph: {
      type: String,
      default: ""
    }
  },

  methods: {
    async selectGraph(graph) {
      this.$emit('showGraph', graph['id']);
    }
  }
}
</script>

<style scoped lang="scss" />