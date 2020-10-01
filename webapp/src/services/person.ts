export class Person {
  constructor (id: number, givenName: string, familyName: string, mailAddress: string | null) {
    this.id = id
    this.givenName = givenName
    this.familyName = familyName
    this.mailAddress = mailAddress
  }

  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

export class Person2 extends Person {
  constructor (id: number, version: number, givenName: string, familyName: string, mailAddress: string | null,
    creationTime: Date, creator: Person | null, deletionTime: Date | null, deletor: Person | null, history: Array<PersonSnapshot>) {
    super(id, givenName, familyName, mailAddress)
    this.version = version
    this.creationTime = creationTime
    this.creator = creator
    this.deletionTime = deletionTime
    this.deletor = deletor
    this.history = history
  }

  version: number;
  creationTime: Date;
  creator: Person | null;
  deletionTime: Date | null;
  deletor: Person | null;
  history: Array<PersonSnapshot>;
}

export class PersonSnapshot {
  constructor (version: number, givenName: string, familyName: string, mailAddress: string | null, editTime: Date, editor: Person | null) {
    this.version = version
    this.givenName = givenName
    this.familyName = familyName
    this.mailAddress = mailAddress
    this.editTime = editTime
    this.editor = editor
  }

  version: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  editTime: Date;
  editor: Person | null;
}

async function getResonse<T> (query: string): Promise<T> {
  const response = await fetch('http://localhost:50805' + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export function getPeople (): Promise<Person[]> {
  return getResonse('/api/people')
}

export function getPerson (id: number): Promise<Person2> {
  return getResonse('/api/person/' + id)
}
